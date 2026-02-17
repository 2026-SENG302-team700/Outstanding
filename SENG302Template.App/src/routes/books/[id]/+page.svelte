<script lang="ts">
	import { page } from '$app/state';
	import type { Book } from '$lib/types';
	import { browser } from '$app/environment';
	import { resolve } from '$app/paths';
	import { fetchWithCsrf } from '$lib/csrf';

	let book: Book | null = $state(null);
	let loading = $state(true);
	let error = $state('');


	async function fetchBook(bId: string): Promise<void> {
		try {
			loading = true;
			error = '';
			const response = await fetchWithCsrf(resolve(`/api/books/${bId}` as any));
			if (!response.ok) {
				if (response.status === 404) {
					throw new Error('Book not found');
				}
				throw new Error('Failed to fetch book details');
			}
			book = await response.json();
		} catch (err) {
			error = (err as Error).message;
			console.error(err);
		} finally {
			loading = false;
		}
	}

	function goBack() {
		if (browser) {
			window.history.back();
		}
	}
	let bookId = $derived(page.params.id);
	// Runs the 'effect' function when the 'bookId' changes (creates a list of dependencies on the fly)
	$effect(() => {
		if (browser && bookId) {
			fetchBook(bookId);
		}
	});
</script>

<div class="container">
	<div class="mb-4">
		<button class="btn btn-secondary" onclick={goBack}>← Back to Books</button>
	</div>

	{#if loading}
		<div class="text-center text-muted py-5">
			<div class="spinner-border" role="status">
				<span class="visually-hidden">Loading...</span>
			</div>
			<p class="mt-3">Loading book details...</p>
		</div>
	{:else if error}
		<div class="alert alert-danger text-center" role="alert">
			<h2 class="alert-heading">Error</h2>
			<p>{error}</p>
			<button class="btn btn-primary mt-2" onclick={goBack}>Go Back</button>
		</div>
	{:else if book}
		<div class="card">
			<div class="card-body">
				<h1 class="card-title text-center mb-4">📖 {book.title}</h1>
				<div class="row g-3">
					<div class="col-12">
						<div class="card border-start border-primary border-4">
							<div class="card-body">
								<div class="row">
									<div class="col-md-3 fw-bold text-muted">Title:</div>
									<div class="col-md-9">{book.title}</div>
								</div>
							</div>
						</div>
					</div>
					<div class="col-12">
						<div class="card border-start border-primary border-4">
							<div class="card-body">
								<div class="row">
									<div class="col-md-3 fw-bold text-muted">Author:</div>
									<div class="col-md-9">{book.author}</div>
								</div>
							</div>
						</div>
					</div>
					<div class="col-12">
						<div class="card border-start border-primary border-4">
							<div class="card-body">
								<div class="row">
									<div class="col-md-3 fw-bold text-muted">Year:</div>
									<div class="col-md-9">{book.year}</div>
								</div>
							</div>
						</div>
					</div>
					<div class="col-12">
						<div class="card border-start border-primary border-4">
							<div class="card-body">
								<div class="row">
									<div class="col-md-3 fw-bold text-muted">Book ID:</div>
									<div class="col-md-9">{book.id}</div>
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	{/if}
</div>
