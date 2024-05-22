import type { Notification } from '$lib/models/notification';
import type { TrackedNotification } from '$lib/models/trackedNotification';

export const notifications: TrackedNotification[] = [];

export function notify(notification: Notification) {
	const timestamp = new Date();
	const hasRead = false;
	const trackedNotification: TrackedNotification = { ...notification, timestamp, hasRead };
	console.log('notify', trackedNotification);
	notifications.push(trackedNotification);
}
