const AWS = require("@aws-sdk/client-rekognition");


addEventListener("fetch", (event) => {
  event.respondWith(
      handleRequest(event.request).catch(
          (err) => new Response(err.stack, { status: 500 })
      )
  );
});

function definitionUrl(md5)
{
  const base = "https://api.github.com/repos/wabbajack-tools/portramatic/contents/Definitions";
  const url = base + "/" + md5.substring(0, 2) + "/" + md5 + "/definition.json";
  return url;
}

// Get the current defintion from Github
async function getDefinition(md5)
{
  const url = definitionUrl(md5);

  const response = await fetch(url,
      {
        method: "GET",
        headers: {
          "Authorization": "token " + GITHUB_PAT,
          "User-Agent": "CloudFlare Worker"
        }
      });
  if (response.status == 404) return null;
  return await response.json();
}

// Write a new definition to GitHub
async function putNewContent(body)
{
  const url = definitionUrl(body.md5);
  const response = await fetch(url,
      {
        method: "PUT",
        headers: {
          "Authorization": "token " + GITHUB_PAT,
          "User-Agent": "CloudFlare Worker"
        },
        body: JSON.stringify({
          content: btoa(JSON.stringify(body, null, 2)),
          message: "Add: " + body.source
        })
      });
  if (response.status != 201) return false;
  return true;
}

function awsCreds() {
    return {
        accessKeyId: AWS_ACCESSKEY,
        secretAccessKey: AWS_SECRETKEY
    }
}

async function labelImage(body)
{
    const imageResponse = await fetch(body.source);
    if (imageResponse.status != 200) return null;
    const imageData = await imageResponse.arrayBuffer();
    console.log(`Loaded ${imageData.byteLength} bytes from source`);

    const rekognition = new AWS.Rekognition({
        region: "us-west-1",
        credentialDefaultProvider: awsCreds
    });

    const allLabels = [];
    
    const moderation = await rekognition.detectModerationLabels({Image: {Bytes: new Uint8Array(imageData)}, MinConfidence: 90})
    console.log(JSON.stringify(moderation));
    if (moderation.ModerationLabels.length != 0) return null;
    
    console.log(Uint8Array.from(imageData));
    const labels = await rekognition.detectLabels({Image: {Bytes: new Uint8Array(imageData)}});
    console.log(JSON.stringify(labels.Labels));
    
    for (var label of labels.Labels)
        allLabels.push(label.Name.toLowerCase());
    
    return allLabels;
}

// Handle the main request 
async function handleRequest(request) {
  const { pathname } = new URL(request.url);

  var body = await (await request.json());
  if (body.version != 2) return new Response("old version", {status: 200});
  
  var labels = await labelImage(body);
  if (labels == null) return new Response("unaccepted", {status: 200});
  labels = [... new Set(labels.concat(body.tags))];
  body.tags = labels;
  console.log(labels);

  var data = await getDefinition(body.md5);

  if (data == null) {
    const result = await putNewContent(body);
    return new Response(definitionUrl(body.md5), {status: result ? 201 : 500});
  }
  else {
    var existingData = await (await fetch(data.download_url)).json();
  }

  return new Response(JSON.stringify(existingData));
}