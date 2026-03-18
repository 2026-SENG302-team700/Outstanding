<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { countries } from "$lib/country/countries";
    import { addToast } from "$lib/toast/toast";
    import { onMount } from 'svelte';

    let user = $state(null);
    let email = $state(localStorage.getItem("email"));
    let initialSeconds = 15;
    let remainingSeconds = $state(initialSeconds);
    let intervalId;
    let timeRemainingText = $state("");
    let errorMessage = $state("")
    let visible = $state(false);
    
    function sendCode() {
        return null;
    }
    
    function formatTime(seconds: number) {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
    }

    onMount(() => {
        intervalId = setInterval(() => {
            if (remainingSeconds > 0) {
                remainingSeconds -= 1;
                timeRemainingText = formatTime(remainingSeconds)
                if (remainingSeconds == initialSeconds-10) {
                   visible=true; 
                }
            } else {
                errorMessage = "One time code has expired"
            }
        }, 1000)
    })

</script>

<div class="d-flex justify-content-center align-items-start vh-100 bg-light">
    <div class="card shadow-sm p-4">
        <h1 class="text-center mb-3">Verify your email address</h1>
        <hr style="height: 2px; background-color: black; width: 50%; margin-left: auto; margin-right: auto;">
        <div class="row justify-content-center">
            <div class="col-md-3">
                <h5>A verification code has been sent to <strong>{email}</strong></h5>
                <p class="small">Please check your inbox and enter the verification code below to verify your email address. The code will expire in <strong>{timeRemainingText}</strong></p>
                    {#if visible}
                        <a role="button" class="text-decoration-underline" on:click={sendCode}>Resend Code</a>
                    {/if}
                <p class="text-danger">{errorMessage}</p>
                <div id="code-input" class="input-group">
                    <input type="text" class="form-control text-center" maxlength="1">
                    <input type="text" class="form-control text-center" maxlength="1">
                    <input type="text" class="form-control text-center" maxlength="1">
                    <input type="text" class="form-control text-center" maxlength="1">
                    <input type="text" class="form-control text-center" maxlength="1">
                    <input type="text" class="form-control text-center" maxlength="1">
                </div>
                <button style="margin-top: 10px" class="btn btn-primary w-100">Confirm registration</button>
            </div>
        </div>
           
    </div>
</div>

<style>
    
</style>
