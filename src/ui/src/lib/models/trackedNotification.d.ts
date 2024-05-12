import type { Notification } from './notification';

export interface TrackedNotification extends Notification {
	timestamp: Date;
	hasRead: boolean;
}
