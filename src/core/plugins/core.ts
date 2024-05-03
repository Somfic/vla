export class MathNode extends ExecutableNode {
    async execute() {
        enum Mode {
            Add,
            Subtract,
            Multiply,
            Divide,
        }

        let mode = this.dropdown("Mode", Object.values(Mode), Mode.Add);

        switch (mode) {
            case Mode.Add:
                this.output("result", this.input("a", 0) + this.input("b", 0));
                break;
            case Mode.Subtract:
                this.output("result", this.input("a", 0) - this.input("b", 0));
                break;
            case Mode.Multiply:
                this.output("result", this.input("a", 0) * this.input("b", 0));
                break;
            case Mode.Divide:
                this.output("result", this.input("a", 0) / this.input("b", 0));
                break;
        }
    }
}
