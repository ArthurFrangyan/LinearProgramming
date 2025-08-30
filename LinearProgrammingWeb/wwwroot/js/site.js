(function(app){
    app.todoStartup = function(){
        document.addEventListener('input', function (event) {
            if (event.target.tagName.toLowerCase() !== 'textarea') return;
            const ta = event.target;
            if (ta.classList.contains('auto-resize')) {
                ta.style.height = 'auto'; // reset
                ta.style.height = ta.scrollHeight + 'px'; // expand
            }
        }, true);

        // Initialize height on page load for pre-filled text
        document.querySelectorAll('textarea.auto-resize').forEach(ta => {
            ta.style.height = ta.scrollHeight + 'px';
        });
    }
})(window.app = window.app || {});