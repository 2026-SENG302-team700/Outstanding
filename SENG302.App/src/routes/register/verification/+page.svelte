<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { countries } from "$lib/country/countries";
    import { addToast } from "$lib/toast/toast";
    import { onMount } from 'svelte';

    let user = $state(null);
    let email = $state("");
    let initialSeconds = 300;
    let remainingSeconds = $state(initialSeconds);
    let intervalId;
    let timeRemainingText = $state("");
    let errorMessage = $state("")
    let resendLinkVisible = $state(false);
    let loading = $state(false);
    let buttonDisabled = $state(false);
    
    let digit1 = $state("");
    let digit2 = $state("");
    let digit3 = $state("");
    let digit4 = $state("");
    let digit5 = $state("");
    let digit6 = $state("");
    

    onMount(() => {
        email = localStorage.getItem("email") ?? "";
        
        if (email) {
            sendCode();
        } else {
            displayError("No email found. Please register again.", false);
        }
    })

    /**
     * Removes any error messages and Resend button before restarting countdown timer
     */
    async function countDownTimer() {
        displayError("", false);
        clearInterval(intervalId);
        remainingSeconds = initialSeconds;
        intervalId = setInterval(() => {
            if (remainingSeconds > 0) {
                remainingSeconds -= 1;
                timeRemainingText = formatTime(remainingSeconds)
                if (remainingSeconds == initialSeconds-10) {
                    resendLinkVisible=true;
                }
            } else {
                buttonDisabled = true;
                displayError("Code is no longer valid, account no longer exists", false);
                checkCode();
            }
        }, 1000)
    }

    /**
     * Format a given number of seconds into a user friendly readable time for the countdown timer
     * @param seconds
     */
    function formatTime(seconds: number) {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
    }

    /**
     * Takes an error message and displays it to the user while also making visible a 'resend code' button
     * @param error
     */
    function displayError(error: string, makeResendLinkVisible: boolean) {
        if (!(resendLinkVisible == makeResendLinkVisible)) resendLinkVisible = makeResendLinkVisible;
        errorMessage = error;
    }

    /**
     * Clears input fields containing digits if code is resent
     */
    function clearInputFields() {
        digit1 = ""
        digit2 = ""
        digit3 = ""
        digit4 = ""
        digit5 = ""
        digit6 = ""
    }

    /**
     * Checks that the user has input only a valid digit in all 6 fields and returns a boolean indicating has done so
     * 
     */
    function inputValidation() {
        
        if (digit1.length === 0 ||
            digit2.length === 0 ||
            digit3.length === 0 ||
            digit4.length === 0 ||
            digit5.length === 0 ||
            digit6.length === 0) {
            displayError("Digits are missing", true);
            return false;
        }
        
        const regex = /^\d{1}$/;
        
        if (!(regex.test(digit1) &&
              regex.test(digit2) && 
              regex.test(digit3) && 
              regex.test(digit4) && 
              regex.test(digit5) && 
              regex.test(digit6))) {
            displayError("You must enter digits", true);
            return false;
        }; 
            
        return true;
    }

    /**
     * Send the one time code to the users email and starts the timer count down
     */
    async function sendCode() {
        try {
            if (remainingSeconds == initialSeconds) {
                await countDownTimer()
            }
            clearInputFields()
            
            loading = true;

            const response = await fetchWithCsrf(resolve(`/api/register/code/generation`), {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    email: email,
                }),
            });
            
            const data = await response.json().catch(() => null);

            if (!response.ok) {
                displayError("Server start time was not received. Internal Server error", true);
                clearInterval(intervalId);
                return;
            }
            
        } catch (err) {
            displayError(err.message, true);
        } 
    }

    /**
     * Checks if the code that the user has entered, matches the one that was sent to their email
     * If it does, the user is redirected to the login page, if not, a corresponding error message is displayed
     */
    async function checkCode() {
        // Only validate input on front end before timeout has occured
        if (remainingSeconds > 0) {
            if (!inputValidation()) return;
        } else {
            clearInterval(intervalId);
        }

        try {
            loading = true;
            
            let userCode = digit1 + digit2 + digit3 + digit4 + digit5 + digit6;

            const response = await fetchWithCsrf(resolve(`/api/register/code/validation`), {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    Email: email,
                    Code: userCode,
                }),
            });

            const data = await response.json().catch(() => null);

            if (!response.ok) {
                if (remainingSeconds > 0) {
                    displayError(data.message, true);
                } else {
                    displayError("Code is no longer valid, account no longer exists", false);
                }
                return;
            } else {
                clearInterval(intervalId);
                addToast("Registration successful. Please log in.", "success");
                goto(resolve(`/login`));
            }
        } catch (err) {
            
            displayError(err.message, true);
        }
    }
    
</script>

<div class="d-flex justify-content-center align-items-start vh-100 bg-light">
    <div class="card shadow-sm p-4">
    <div class="mb-3">
        <button
            type="button"
            class="btn btn-secondary"
            on:click={() => goto(resolve("/register"))}>Cancel</button
        >
    </div>
        <h1 class="text-center mb-3">Verify your email address</h1>
        <hr style="height: 2px; background-color: black; width: 50%; margin-left: auto; margin-right: auto;">
        <div class="row justify-content-center">
            <div class="col-md-3">
                <h5>A verification code has been sent to <strong>{email}</strong></h5>
                <p class="small">Please check your inbox and enter the verification code below to verify your email address. The code will expire in <strong>{timeRemainingText}</strong></p>
                {#key resendLinkVisible}
                    {#if resendLinkVisible}
                        <a role="button" class="text-decoration-underline" on:click={sendCode}>Resend Code</a>
                    {/if}
                {/key}
                <p class="text-danger">{errorMessage}</p>
                <div id="code-input" class="input-group">
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={digit1}>
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={digit2}>
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={digit3}>
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={digit4}>
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={digit5}>
                    <input type="text" class="form-control text-center" maxlength="1" bind:value={digit6}>
                </div>
                {#key buttonDisabled}
                    <button style="margin-top: 10px" class="btn btn-primary w-100" on:click={checkCode} disabled={buttonDisabled}>Confirm registration</button>
                {/key}
            </div>
        </div>
           
    </div>
</div>

<style>
    
</style>
