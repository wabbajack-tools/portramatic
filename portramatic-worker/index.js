
addEventListener("fetch", (event) => {
    event.respondWith(
        handleRequest(event.request).catch(
            (err) => new Response(err.stack, {status: 500})
        )
    );
});


function deepEqual(object1, object2) {
    const keys1 = Object.keys(object1);
    const keys2 = Object.keys(object2);
    if (keys1.length !== keys2.length) {
        return false;
    }
    for (const key of keys1) {
        const val1 = object1[key];
        const val2 = object2[key];
        const areObjects = isObject(val1) && isObject(val2);
        if (
            areObjects && !deepEqual(val1, val2) ||
            !areObjects && val1 !== val2
        ) {
            return false;
        }
    }
    return true;
}
function isObject(object) {
    return object != null && typeof object === 'object';
}

function diff(a, b) { 
    b = new Set(b);
    return new Set([...a].filter(x => !b.has(x)));
}

function definitionUrl(md5) {
    const base = "https://api.github.com/repos/wabbajack-tools/portramatic/contents/Definitions";
    const url = base + "/" + md5.substring(0, 2) + "/" + md5 + "/definition.json";
    return url;
}

// Get the current defintion from Github
async function getDefinition(md5) {
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
async function putNewContent(body) {
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

async function putUpdateContent(body, sha) {
    const url = definitionUrl(body.md5);
    const response = await fetch(url,
        {
            method: "PUT",
            headers: {
                "Authorization": "token " + GITHUB_PAT,
                "User-Agent": "CloudFlare Worker"
            },
            body: JSON.stringify({
                sha: sha,
                content: btoa(JSON.stringify(body, null, 2)),
                message: "Updated: " + body.source
            })
        });
    if (response.status != 201) return false;
    return true;
}

// Handle the main request 
async function handleRequest(request) {
    const {pathname} = new URL(request.url);

    var body = await (await request.json());
    if (body.version != 2) return new Response("old version", {status: 200});


    var data = await getDefinition(body.md5);

    if (data == null) {
        
        if (body.source.includes("xxx") || body.source.includes("gelbooru.com")) 
            return new Response("unaccepted", {status: 200});
        
        const result = await putNewContent(body);
        return new Response(definitionUrl(body.md5), {status: result ? 201 : 500});
    } else {
        var existingData = await (await fetch(data.download_url)).json();
        const oldDataString = JSON.stringify(existingData);

        existingData.tags = [... new Set(existingData.tags.concat(body.tags))];
        existingData.small = body.small;
        existingData.medium = body.medium;
        existingData.full = body.full;
        
        if (oldDataString == JSON.stringify(existingData))
            return new Response("not updated", {status: 200});

        //await putUpdateContent(existingData, data.sha);
        return new Response("updated", {status: 200})
    }

    return new Response(JSON.stringify(existingData));
}