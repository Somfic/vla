<script lang="ts">
    import type { NodeData, BrickArgument } from "$lib/core";
    import { saveNodeChanges } from "$lib/api";
    import Input from "../../Input.svelte";

    interface Props {
        argument: BrickArgument;
        data: NodeData;
    }

    let { argument, data }: Props = $props();

    function getDefaultValue(type: string, defaultValue: string | null) {
        if (defaultValue !== null) {
            try {
                return JSON.parse(defaultValue);
            } catch {
                // If parsing fails, return the string as-is for string types
                if (type.toLowerCase() === "string") {
                    return defaultValue;
                }
            }
        }

        // Fallback to type-based defaults
        switch (type.toLowerCase()) {
            case "boolean":
                return false;
            case "number":
                return 0;
            case "string":
                return "";
            default:
                return null;
        }
    }

    function getValue(argumentId: string, type: string, defaultValue: string | null) {
        const value = data.arguments[argumentId];
        if (value === undefined) {
            return getDefaultValue(type, defaultValue);
        }
        try {
            return JSON.parse(value);
        } catch {
            return getDefaultValue(type, defaultValue);
        }
    }


    function setValue(argumentId: string, value: any) {
        // Create a new arguments object to trigger reactivity
        data.arguments = {
            ...data.arguments,
            [argumentId]: JSON.stringify(value)
        };
        saveNodeChanges();
    }

    function mapArgumentType(
        type: string,
    ): "boolean" | "number" | "string" | "enum" {
        switch (type.toLowerCase()) {
            case "boolean":
                return "boolean";
            case "number":
                return "number";
            case "string":
                return "string";
            case "enum":
                return "enum";
            default:
                return "string";
        }
    }

</script>

<div class="argument">
    <Input
        value={getValue(argument.id, argument.type, argument.defaultValue)}
        type={mapArgumentType(argument.type)}
        enumOptions={argument.enumOptions}
        onchange={(value) => setValue(argument.id, value)}
        id={argument.id}
        label={argument.label}
    />
</div>

<style lang="scss">
    .argument {
        width: 100%;
    }
</style>
