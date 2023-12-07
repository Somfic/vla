import { test, expect } from "@playwright/test";

test("splotlight show sup", async ({ page }) => {
    await page.goto("http://localhost:1421/");
    await page.locator("section").click();
    await page.press("section", "Space");
    await page.getByPlaceholder("Search for anything");
    await page.getByPlaceholder("Search for anything").press("Escape");
    await page.locator("section").click();
});
