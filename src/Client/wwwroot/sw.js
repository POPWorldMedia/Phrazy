self.addEventListener('install', event => event.waitUntil(onInstall(event)));

async function onInstall(event) {
	self.skipWaiting();
}

self.addEventListener('fetch', event => {
});

if ('sync' in self.registration)
	self.registration.sync.register("dosync");

const broadcast = new BroadcastChannel("p");
self.addEventListener('sync', function (event) {
	console.log("sync fired");
	event.waitUntil(broadcast.postMessage({ payload: "do" }));
});
