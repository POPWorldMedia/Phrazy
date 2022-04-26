self.addEventListener('install', event => event.waitUntil(onInstall(event)));

async function onInstall(event) {
	self.skipWaiting();
}

self.addEventListener('fetch', event => {
});