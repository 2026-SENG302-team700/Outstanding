<script lang="ts">
    import { CollisionPriority } from "@dnd-kit/abstract";
    import { createSortable } from "@dnd-kit/svelte/sortable";
    import SortableItem from "./SortableItem.svelte";

    let {
        id,
        index,
        rows,
        colors,
    }: {
        id: string;
        index: number;
        rows: string[];
        colors: Record<string, string>;
    } = $props();

    const sortable = createSortable({
        get id() {
            return id;
        },
        get index() {
            return index;
        },
        accept: ["column", "item"],
        collisionPriority: CollisionPriority.Low,
        type: "column",
    });
</script>

<div
    class="container"
    {@attach sortable.attach}
    data-shadow={sortable.isDragging ? "true" : undefined}
>
    <h2>
        {id}
    </h2>
    <ul>
        <!--populates the column with it's items.-->
        {#each rows as itemId, itemIndex (itemId)}
            <SortableItem
                id={itemId}
                column={id}
                index={itemIndex}
                accentColor={colors[id]}
            />
        {/each}
    </ul>
</div>
