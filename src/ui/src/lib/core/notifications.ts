import type { Notification } from '$lib/models/notification';
import type { TrackedNotification } from '$lib/models/trackedNotification';

export const notifications: TrackedNotification[] = [
	{
		id: 1,
		timestamp: new Date(),
		level: 'information',
		title: 'Welcome to the app!',
		body: 'You can now',
		hasRead: false
	}
];

export function notify(notification: Notification) {
	const timestamp = new Date();
	const hasRead = false;
	const trackedNotification: TrackedNotification = { ...notification, timestamp, hasRead };
	console.log('notify', trackedNotification);
	notifications.push(trackedNotification);
}
