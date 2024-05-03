class Engine {
    constructor() {}
}

abstract class ExecutableNode {
    constructor() {
        this.id = Math.random().toString(36).substring(2, 15);
    }

    id: string;

    abstract execute(): Promise<void>;

    dropdown<T>(name: string, possible_values: T[], default_value: T): T {
        return default_value;
    }

    input<T>(name: string, default_value: T): T {
        // TODO: implement
        return default_value;
    }

    output<T>(name: string, value: T): void {
        // TODO: implement
    }
}
