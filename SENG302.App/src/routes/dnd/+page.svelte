<script lang="ts">
    import {
        draggable,
        droppable,
        type DragDropState,
    } from "@thisux/sveltednd";

    interface Card {
        id: string;
        title: string;
        status: "todo" | "in-progress" | "done";
    }

    let cards = $state<Card[]>([
        { id: "1", title: "Task A", status: "todo" },
        { id: "2", title: "Task B", status: "in-progress" },
        { id: "3", title: "Task C", status: "done" },
    ]);

    const columns = ["todo", "in-progress", "done"] as const;

    function handleDrop(state: DragDropState<Card>) {
        const { draggedItem, targetContainer } = state;
        if (!targetContainer) return;

        cards = cards.map((c) =>
            c.id === draggedItem.id
                ? { ...c, status: targetContainer as Card["status"] }
                : c,
        );
    }
</script>

<div
    class="board"
    style="display: grid; grid-template-columns: 20% 20% 20%; column-gap: 10%;"
>
    {#each columns as column}
        <div
            use:droppable={{
                container: column,
                callbacks: { onDrop: handleDrop },
            }}
            class="column"
        >
            <h3>{column}</h3>
            {#each cards.filter((c) => c.status === column) as card (card.id)}
                <div
                    use:draggable={{ container: column, dragData: card }}
                    class="card"
                >
                    {card.title}
                </div>
            {/each}
        </div>
    {/each}
</div>
