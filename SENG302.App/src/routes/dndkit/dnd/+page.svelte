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

    const todoItems = ["buy milk", "make bed", "look for jobs", "go to sleep"];

    //defines colours based on column name
    const COLORS: Record<string, string> = {
        column: "#FF851B",
    };

    /**
     * sensors are used to detect user input via mouse, touch and keyboard.
     * PointerSensor is for mouse, keyboard and stylus, KeyboardSensor is self explanatory.
     * activatorElements are the objects that the Pointer can initiate a drag operation with. (in our case, its
     * the grab handles on the task.)
     */
    const sensors = [
        PointerSensor.configure({
            activatorElements(source) {
                return [source.element, source.handle];
            },
        }),
        KeyboardSensor,
    ];

    //maps the column to it's initial items
    const initialItems: Record<string, string[]> = {
        column: todoItems.map((id) => `${id}`),
    };

    //converts the initial items into a reactive dictionary
    let items = $state<Record<string, string[]>>(initialItems);

    //Record is stored as a key value pair, and this creates an array of all the columns
    const columns = Object.keys(initialItems);

    //creates a copy of the board before any movements have been made
    let snapshot = $state(structuredClone(initialItems));

    /**
     * creates a new copy of the snapshot every time a task item is dragged
     */
    function onDragStart() {
        snapshot = structuredClone(items);
    }

    /**
     * takes an event (drag operation). then checks what it is dragging,
     * if it is a column nothing happens, but calls the "move" function
     * if it is an item.
     * @param event
     */
    function onDragOver(event: any) {
        const { source } = event.operation;
        if (source && source.type === "column") return;
        items = move(items, event);
    }

    /**
     * if the drag event is cancelled, then revert back to the original copy
     * @param event
     */
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
