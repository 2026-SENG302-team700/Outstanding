import { readFileSync, writeFileSync, readdirSync, statSync } from 'fs';
import { join } from 'path';

// Recursively find all HTML files in a directory
function findHtmlFiles(dir, fileList = []) {
  const files = readdirSync(dir);

  files.forEach(file => {
    const filePath = join(dir, file);
    if (statSync(filePath).isDirectory()) {
      findHtmlFiles(filePath, fileList);
    } else if (file.endsWith('.html')) {
      fileList.push(filePath);
    }
  });

  return fileList;
}

// Find all HTML files in the build directory
const htmlFiles = findHtmlFiles('build');

htmlFiles.forEach(file => {
  let html = readFileSync(file, 'utf-8');

  // Add nonce to inline script tags (scripts without src attribute)
  // Match <script> or <script type="..."> but not <script src="...">
  html = html.replace(
    /<script(?![^>]*\bsrc=)([^>]*)>/g,
    (match, attributes) => {
      // Don't add nonce if it already exists
      if (attributes.includes('nonce=')) {
        return match;
      }
      return `<script${attributes} nonce="__CSP_NONCE__">`;
    }
  );

  // Add nonce to inline style tags
  html = html.replace(
    /<style([^>]*)>/g,
    (match, attributes) => {
      // Don't add nonce if it already exists
      if (attributes.includes('nonce=')) {
        return match;
      }
      return `<style${attributes} nonce="__CSP_NONCE__">`;
    }
  );

  writeFileSync(file, html, 'utf-8');
});

console.log(`✓ Added nonce placeholders to ${htmlFiles.length} HTML file(s)`);
