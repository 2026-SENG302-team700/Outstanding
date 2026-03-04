/**
 * ISO 3166 Country Dataset
 * 
 * This file contains a static locally stored list of countries using ISO 3166
 * alpha-2 country codes and their names.
 */

export type CountryCode = 
| "NZ" | "AU" | "US" | "GB" | "CA" | "JP" | "DE" | "FR"
| "CN" | "IN" | "BR" | "ZA" | "SG" | "KR" | "MX" | "IT"
| "ES" | "NL" | "SE" | "CH" | "RO";

export interface Country {
    code: CountryCode;
    name: string;
}

export const countries: Country[] = [
    { code: "NZ", name: "New Zealand" },
    { code: "AU", name: "Australia" },
    { code: "US", name: "United States" },
    { code: "GB", name: "United Kingdom" },
    { code: "CA", name: "Canada" },
    { code: "JP", name: "Japan" },
    { code: "DE", name: "Germany" },
    { code: "FR", name: "France" },
    { code: "CN", name: "China" },
    { code: "IN", name: "India" },
    { code: "BR", name: "Brazil" },
    { code: "ZA", name: "South Africa" },
    { code: "SG", name: "Singapore" },
    { code: "KR", name: "South Korea" },
    { code: "MX", name: "Mexico" },
    { code: "IT", name: "Italy" },
    { code: "ES", name: "Spain" },
    { code: "NL", name: "Netherlands" },
    { code: "SE", name: "Sweden" },
    { code: "CH", name: "Switzerland" },
    { code: "RO", name: "Romania" },
];