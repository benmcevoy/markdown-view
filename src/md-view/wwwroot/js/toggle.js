// Raw/Render Toggle functionality
(function() {
    'use strict';

    const toggleCheckbox = document.getElementById('raw-render-toggle');
    const rawContentDiv = document.getElementById('raw-content');
    const renderedContentDiv = document.getElementById('rendered-content');
    const currentPathEl = document.getElementById('current-path');

    if (!toggleCheckbox || !rawContentDiv || !renderedContentDiv) {
        return;
    }

    // Toggle between raw and rendered views
    toggleCheckbox.addEventListener('change', function(e) {
        const isRawMode = e.target.checked;

        if (isRawMode) {
            // Show raw markdown with line numbers
            rawContentDiv.style.display = 'block';
            renderedContentDiv.style.display = 'none';

            // Add line numbers to code blocks
            addLineNumbers(rawContentDiv);
        } else {
            // Show rendered markdown
            rawContentDiv.style.display = 'none';
            renderedContentDiv.style.display = 'block';
        }
    });

    // Add line numbers to code blocks in raw mode
    function addLineNumbers(container) {
        const codeBlocks = container.querySelectorAll('pre code');

        codeBlocks.forEach(function(codeBlock) {
            const pre = codeBlock.parentElement;
            const lines = codeBlock.textContent.split('\n').length;

            // Create line number container
            const lineNumbers = document.createElement('div');
            lineNumbers.className = 'line-numbers';
            lineNumbers.style.width = '30px';
            lineNumbers.style.textAlign = 'right';
            lineNumbers.style.color = '#6a9955';
            lineNumbers.style.fontFamily = "'Courier New', Consolas, monospace";
            lineNumbers.style.fontSize = '0.85rem';
            lineNumbers.style.lineHeight = '1.5';

            // Create line number elements
            for (let i = 1; i <= lines; i++) {
                const span = document.createElement('span');
                span.textContent = i;
                span.className = 'line-number';
                lineNumbers.appendChild(span);
            }

            // Insert line numbers before code block
            pre.insertBefore(lineNumbers, codeBlock);
        });
    }

    // Syntax highlighting for code blocks
    function highlightSyntax(container) {
        const codeBlocks = container.querySelectorAll('pre code');

        codeBlocks.forEach(function(codeBlock) {
            const text = codeBlock.textContent;
            const highlighted = highlightSyntaxText(text);
            codeBlock.innerHTML = highlighted;
        });
    }

    // Simple syntax highlighting for common patterns
    function highlightSyntaxText(text) {
        // Escape HTML first
        let escaped = escapeHtml(text);

        // Highlight comments (markdown-style: <!-- comment -->)
        escaped = escaped.replace(/<!--[\s\S]*?-->/g, '<span class="token-comment">$&</span>');

        // Highlight strings (markdown-style: "string" or 'string')
        escaped = escaped.replace(/(["'`])(?:(?=(\\?))\2|.)[^\\]*\1/g, '<span class="token-string">$&</span>');

        // Highlight keywords (markdown-style: !important, etc.)
        escaped = escaped.replace(/\b(!important|true|false|null)\b/g, '<span class="token-keyword">$&</span>');

        // Highlight functions (simple heuristic)
        escaped = escaped.replace(/(\b[a-zA-Z_][a-zA-Z0-9_]*)\s*\(/g, '<span class="token-function">$&</span>(');

        // Highlight variables
        escaped = escaped.replace(/\b([a-zA-Z_][a-zA-Z0-9_]*)\b/g, '<span class="token-variable">$&</span>');

        // Highlight numbers
        escaped = escaped.replace(/\b(\d+)\b/g, '<span class="token-number">$&</span>');

        return escaped;
    }

    // Escape HTML special characters
    function escapeHtml(text) {
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, function(m) {
            return map[m];
        });
    }

    // Initialize syntax highlighting when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            highlightSyntax(rawContentDiv);
        });
    } else {
        highlightSyntax(rawContentDiv);
    }
})();
