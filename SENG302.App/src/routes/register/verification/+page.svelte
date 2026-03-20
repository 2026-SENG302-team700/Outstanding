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
    
    let digit1 = $state("");
    let digit2 = $state("");
    let digit3 = $state("");
    let digit4 = $state("");
    let digit5 = $state("");
    let digit6 = $state("");
    
    let serverStartTime;

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

    function formatTime(seconds: number) {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
    }
    
    function inputValidation() {
        
        if (digit1.length === 0 ||
            digit2.length === 0 ||
            digit3.length === 0 ||
            digit4.length === 0 ||
            digit5.length === 0 ||
            digit6.length === 0) {
            errorMessage = "Digits are missing";
            visible = true;
            return false;
        }
        
        const regex = /^\d{1}$/;
        
        if (!(regex.test(digit1) &&
              regex.test(digit2) && 
              regex.test(digit3) && 
              regex.test(digit4) && 
              regex.test(digit5) && 
              regex.test(digit6)) {
            errorMessage = "You must enter digits";
            visible = true;
            return false;
        }) 
            
        return true;
    }

    async function sendCode() {
        
        try {
            loading = true;

            const response = await fetchWithCsrf(resolve(`/api/register/code/generate`), {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    email: email,
                }),
            });

            const data = await response.json().catch(() => null);

            if (!response.ok) {
                visible = true;
                errorMessage = "Server start time was not received. Internal Server error";
                return;
            }
        } catch (err e) {
            errorMessage = e
            visible = true;
        } 
    }

    function checkCode() {
        try {
            loading = true;

            const response = await fetchWithCsrf(resolve(`/api/register/code/validate`), {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    email: email,
                }),
            });

            const data = await response.json().catch(() => null);

            if (!response.ok) {
                visible = true;
                errorMessage = "Server start time was not received. Internal Server error";
                return;
            }
        } catch (err e) {
            errorMessage = e
            visible = true;
        }
    }
    

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
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={}>
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={}>
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={}>
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={}>
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={}>
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={}>
                </div>
                <button style="margin-top: 10px" class="btn btn-primary w-100" on:click={checkCode}>Confirm registration</button>
            </div>
        </div>
           
    </div>
</div>

<style>
    
</style>
