/**
 * formats the string based on the users locale
 */
export function formatDate(dateString: string) {
    const date = new Date(dateString);
    return date.toLocaleDateString();
}