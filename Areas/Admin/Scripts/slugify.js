window.Slugify = (function () {
    function removeDiacritics(str) {
        return str.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    }
    function makeSlug(text) {
        if (!text) return "";
        let s = text.toString().trim().toLowerCase();
        s = removeDiacritics(s);
        s = s.replace(/[^a-z0-9\s-]/g, '')  // bỏ ký tự đặc biệt
            .replace(/\s+/g, '-')          // space -> -
            .replace(/-+/g, '-')           // gộp -
            .replace(/^-+|-+$/g, '');      // trim -
        return s;
    }
    function bind(nameSelector, slugSelector) {
        var name = document.querySelector(nameSelector);
        var slug = document.querySelector(slugSelector);
        if (!name || !slug) return;

        const update = () => {
            if (!slug.value || slug.dataset.auto == "1") {
                slug.value = makeSlug(name.value);
                slug.dataset.auto = "1";
            }
        };
        name.addEventListener('input', update);
        slug.addEventListener('input', () => { slug.dataset.auto = "0"; });
        update();
    }
    return { bind, makeSlug };
})();
