(function () {

    function initEditors() {
        document.querySelectorAll(".ckeditor").forEach(editor => {
            // Щоб не ініціалізувати повторно
            if (editor.dataset.ckeditorInitialized)
                return;

            editor.dataset.ckeditorInitialized = "true";

            ClassicEditor
                .create(editor, {
                    ckfinder: {
                        uploadUrl: '/Product/UploadImageForCKEditor'
                    }
                })
                .catch(error => console.error(error));
        });
    }

    function loadCKEditor() {
        if (window.ClassicEditor) {
            initEditors();
            return;
        }

        const script = document.createElement("script");
        script.src = "https://cdn.ckeditor.com/ckeditor5/39.0.0/classic/ckeditor.js";
        script.onload = initEditors;
        script.onerror = () => console.error("Не вдалося завантажити CKEditor.");

        document.head.appendChild(script);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", loadCKEditor);
    } else {
        loadCKEditor();
    }

})();


/*document.addEventListener("DOMContentLoaded", function () {
    const editorElement = document.querySelector("#Description");

    if (editorElement) {
        ClassicEditor
            .create(editorElement)
            .catch(error => {
                console.error(error);
            });
    }
});*/