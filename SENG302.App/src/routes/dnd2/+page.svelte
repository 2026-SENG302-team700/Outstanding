<script lang="ts">
    import {
        DragDropProvider,
        PointerSensor,
        KeyboardSensor,
    } from "@dnd-kit/svelte";
    import { defaultPreset } from "@dnd-kit/dom";
    import { move } from "@dnd-kit/helpers";
    import "./styles.css";
    import SortableColumn from "./SortableColumn.svelte";

    function createRange(length: number) {
        return Array.from({ length }, (_, i) => i + 1);
    }

    const ITEM_COUNT = 6;

    const COLORS: Record<string, string> = {
        todo: "#FF851B",
        inProgress: "#7193f1",
        done: "#2ECC40",
    };

    const sensors = [
        PointerSensor.configure({
            activatorElements(source) {
                return [source.element, source.handle];
            },
        }),
        KeyboardSensor,
    ];

    const initialItems: Record<string, string[]> = {
        todo: createRange(5).map((id) => `task example ${id}`),
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
    <div class="wrapper" style="width: 340px">
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
