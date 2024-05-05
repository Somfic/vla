export interface Extension {
    metadata(): ExtensionMetadata;

    on_start(handle: AppHandle): Promise<void>;
    on_stop?(handle: AppHandle): Promise<void>;
}

export interface ExtensionMetadata {
    name: string;
    version: string;
    description: string;
}

export interface AppHandle {
    log(message: string): void;
}
