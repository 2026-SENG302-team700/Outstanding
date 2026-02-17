<script lang="ts">
	import { onMount } from 'svelte';
	import type { Book } from '$lib/types';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import { fetchWithCsrf } from '$lib/csrf';


	let books: Book[] = $state([]);
	let title = $state('');
	let author = $state('');
	let year = $state('');
	let loading = $state(false);
	let error = $state('');

	onMount(() => {
		fetchBooks();
	});

	async function fetchBooks() {
		try {
			loading = true;
			error = '';
			const response = await fetchWithCsrf(resolve(`/api/books` as any));
			if (!response.ok) {
				throw new Error('Failed to fetch books');
			}
			books = await response.json();
		} catch (err) {
			error = 'Failed to load books: ' + (err as Error).message;
			console.error(err);
		} finally {
			loading = false;
		}
	}

	async function addBook() {
		if (!title || !author || !year) {
			error = 'Please fill in all fields';
			return;
		}

		try {
			loading = true;
			error = '';
			const response = await fetchWithCsrf(resolve(`/api/books` as any), {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json'
				},
				body: JSON.stringify({
					title,
					author,
					year: parseInt(year)
				})
			});

			if (!response.ok) {
				throw new Error('Failed to add book');
			}

			const newBook = await response.json();
			books = [...books, newBook];

			// Clear form
			title = '';
			author = '';
			year = '';
		} catch (err) {
			error = 'Failed to add book: ' + (err as Error).message;
			console.error(err);
		} finally {
			loading = false;
		}
	}

	async function deleteBook(id: number) {
		try {
			loading = true;
			error = '';
			const response = await fetchWithCsrf(resolve(`/api/books/${id}` as any), {
				method: 'DELETE'
			});

			if (!response.ok) throw new Error('Failed to delete book');

			books = books.filter((b) => b.id !== id);
		} catch (err) {
			error = 'Failed to delete book: ' + (err as Error).message;
			console.error(err);
		} finally {
			loading = false;
		}
	}
</script>

<div class="container">
	<h1 class="text-center mb-4">📚 Books Manager</h1>

	{#if error}
		<div class="alert alert-danger" role="alert">{error}</div>
	{/if}

	<div class="card mb-4">
		<div class="card-body">
			<h2 class="card-title h5 mb-3">Add New Book</h2>
			<form onsubmit={addBook} class="row g-3">
				<div class="col-md-4">
					<input type="text" class="form-control" placeholder="Title" bind:value={title} disabled={loading} />
				</div>
				<div class="col-md-4">
					<input type="text" class="form-control" placeholder="Author" bind:value={author} disabled={loading} />
				</div>
				<div class="col-md-2">
					<input type="number" class="form-control" placeholder="Year" bind:value={year} disabled={loading} />
				</div>
				<div class="col-md-2">
					<button type="submit" class="btn btn-primary w-100" disabled={loading}>
						{loading ? 'Adding...' : 'Add Book'}
					</button>
				</div>
			</form>
		</div>
	</div>

	<div class="card">
		<div class="card-body">
			<h2 class="card-title h5 mb-3">Books Collection</h2>
			{#if loading && books.length === 0}
				<div class="text-center text-muted py-4">Loading books...</div>
			{:else if books.length === 0}
				<div class="text-center text-muted py-4">No books yet. Add your first book above!</div>
			{:else}
				<div class="table-responsive">
					<table class="table table-hover">
						<thead>
							<tr>
								<th>Title</th>
								<th>Author</th>
								<th>Year</th>
								<th>Actions</th>
							</tr>
						</thead>
						<tbody>
							{#each books as book (book.id)}
								<tr class="cursor-pointer" onclick={() => goto(resolve(`/books/${book.id}`))}>
									<td>{book.title}</td>
									<td>{book.author}</td>
									<td>{book.year}</td>
									<td>
										<button
											class="btn btn-danger btn-sm"
											onclick={((e) => {e.stopPropagation(); deleteBook(book.id)})}
											disabled={loading}
										>
											Delete
										</button>
									</td>
								</tr>
							{/each}
						</tbody>
					</table>
				</div>
			{/if}
		</div>
	</div>
</div>

<style>
	.cursor-pointer {
		cursor: pointer;
	}
</style>
