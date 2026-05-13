<script lang="ts">
    /**
     * Start a new timer for the resend button to ensure the user cant spam their email
     */
    function startResendCountdown() {
        resendTimer = 30;
        const interval = setInterval(() => {
            resendTimer--;
            if (resendTimer <= 0) clearInterval(interval);
        }, 1000);
    }

    /**
     * Format a given number of seconds into a user friendly readable time for the countdown timer
     * @param seconds
     */
    function formatTime(seconds: number) {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins.toString().padStart(2, "0")}:${secs.toString().padStart(2, "0")}`;
    }
</script>
