// Toggle between raw and rendered markdown view
document.addEventListener('DOMContentLoaded', function() {
  const rawToggle = document.getElementById('raw-toggle');
  const rawContent = document.getElementById('raw-content');
  const renderedContent = document.getElementById('rendered-content');

  if (rawToggle && rawContent && renderedContent) {
    let isRaw = true;

    rawToggle.addEventListener('click', function() {
      isRaw = !isRaw;

      if (isRaw) {
        rawContent.style.display = 'block';
        renderedContent.style.display = 'none';
      } else {
        rawContent.style.display = 'none';
        renderedContent.style.display = 'block';
      }
    });
  }
});
