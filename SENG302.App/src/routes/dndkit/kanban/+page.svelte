<script lang="ts">
    import {
        DragDropProvider,
        PointerSensor,
        KeyboardSensor,
    } from "@dnd-kit/svelte";
    import { defaultPreset } from "@dnd-kit/dom";
    import { move } from "@dnd-kit/helpers";
    import "../styles.css";
    import SortableColumn from "../SortableColumn.svelte";

    const COLORS: Record<string, string> = {
        "To Do": "#FF851B",
        "In Progress": "#7193f1",
        Done: "#2ECC40",
    };

    const sensors = [
        PointerSensor.configure({
            activatorElements(source) {
                return [source.element, source.handle];
            },
        }),
        KeyboardSensor,
    ];
    const todoItems = ["buy milk", "make bed", "look for jobs"];
    const progressItems = ["complete sprint 3", "procrastinate"];
    const doneItems = ["do nothing", "complete sprint 2", "complete sprint 1"];

    const initialItems: Record<string, string[]> = {
        "To Do": todoItems.map((id) => `${id}`),
        "In Progress": progressItems.map((id) => `${id}`),
        Done: doneItems.map((id) => `${id}`),
    };

    let items = $state<Record<string, string[]>>(initialItems);

    const columns = Object.keys(initialItems);
    let snapshot = $state(structuredClone(initialItems));

    function onDragStart() {
        snapshot = structuredClone(items);
    }

    function onDragOver(event: any) {
        const { source } = event.operation;
        if (source && source.type === "column") return;
        items = move(items, event);
    }

    function onDragEnd(event: any) {
        if (event.canceled) {
            items = snapshot;
        }
    }
</script>

<DragDropProvider
    plugins={defaultPreset.plugins}
    {sensors}
    {onDragStart}
    {onDragOver}
    {onDragEnd}
>
    <div class="wrapper">
        {#each columns as column, columnIndex (column)}
            <SortableColumn
                id={column}
                index={columnIndex}
                rows={items[column]}
                colors={COLORS}
            />
        {/each}
    </div>
</DragDropProvider>
