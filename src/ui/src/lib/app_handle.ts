import type { AppHandle } from '../../../extensions/abstractions/abstractions';

export default class TestAppHandle implements AppHandle {
	messages: string[] = [];

	log(message: string): void {
		this.messages.push(message);
	}

	getMessages(): string[] {
		return this.messages;
	}
}
