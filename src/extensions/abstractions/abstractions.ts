export abstract class Extension {
    abstract on_start(handle: AppHandle): Promise<void>;
    abstract on_stop(handle: AppHandle): Promise<void>;
}

export interface AppHandle {
    log(message: string): void;
}
