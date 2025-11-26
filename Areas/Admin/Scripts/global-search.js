// === GLOBAL SEARCH SCRIPT ===
// File: ~/Scripts/global-search.js

class GlobalSearch {
    constructor() {
        this.initGlobalSearch();
        this.initSearchSuggestions();
    }

    initGlobalSearch() {
        const searchForm = document.getElementById('globalSearchForm');
        const searchInput = document.getElementById('globalSearchInput');
        const searchBtn = document.getElementById('globalSearchBtn');

        if (searchForm && searchInput) {
            // Xử lý submit form
            searchForm.addEventListener('submit', (e) => {
                this.handleSearch(e, searchInput.value);
            });

            // Xử lý nhấn Enter
            searchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    this.handleSearch(e, searchInput.value);
                }
            });

            // Xử lý click nút search
            if (searchBtn) {
                searchBtn.addEventListener('click', (e) => {
                    this.handleSearch(e, searchInput.value);
                });
            }

            // Focus vào search input
            searchInput.focus();
        }
    }

    handleSearch(e, searchTerm) {
        if (!searchTerm || searchTerm.trim() === '') {
            e.preventDefault();
            this.showNotification('Vui lòng nhập từ khóa tìm kiếm', 'warning');
            return;
        }

        // Hiệu ứng loading
        this.showLoading();

        // Form sẽ tự submit với action đã định sẵn
        // Không cần chuyển hướng thủ công
    }

    initSearchSuggestions() {
        const searchInput = document.getElementById('globalSearchInput');

        if (!searchInput) return;

        let suggestionTimeout;

        searchInput.addEventListener('input', (e) => {
            const value = e.target.value.trim();

            // Clear previous timeout
            clearTimeout(suggestionTimeout);

            if (value.length > 2) {
                // Debounce search suggestions
                suggestionTimeout = setTimeout(() => {
                    this.fetchSearchSuggestions(value);
                }, 300);
            } else {
                this.hideSuggestions();
            }
        });

        // Click outside to hide suggestions
        document.addEventListener('click', (e) => {
            if (!e.target.closest('#globalSearchForm')) {
                this.hideSuggestions();
            }
        });

        // Prevent form submit when selecting suggestion
        searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
                e.preventDefault();
                this.handleSuggestionNavigation(e.key);
            }
        });
    }

    fetchSearchSuggestions(query) {
        // Gọi API để lấy gợi ý tìm kiếm
        fetch(`/Products/GetSearchSuggestions?query=${encodeURIComponent(query)}`)
            .then(response => response.json())
            .then(suggestions => {
                this.showSearchSuggestions(suggestions);
            })
            .catch(error => {
                console.error('Error fetching search suggestions:', error);
            });
    }

    showSearchSuggestions(suggestions) {
        this.hideSuggestions();

        if (!suggestions || suggestions.length === 0) return;

        const searchForm = document.getElementById('globalSearchForm');
        const suggestionsContainer = document.createElement('div');
        suggestionsContainer.id = 'searchSuggestions';
        suggestionsContainer.className = 'search-suggestions';

        suggestions.forEach(suggestion => {
            const suggestionItem = document.createElement('div');
            suggestionItem.className = 'suggestion-item';
            suggestionItem.textContent = suggestion;
            suggestionItem.addEventListener('click', () => {
                document.getElementById('globalSearchInput').value = suggestion;
                this.hideSuggestions();
                document.getElementById('globalSearchForm').submit();
            });
            suggestionsContainer.appendChild(suggestionItem);
        });

        searchForm.appendChild(suggestionsContainer);
    }

    hideSuggestions() {
        const existingSuggestions = document.getElementById('searchSuggestions');
        if (existingSuggestions) {
            existingSuggestions.remove();
        }
    }

    handleSuggestionNavigation(key) {
        const suggestions = document.querySelectorAll('.suggestion-item');
        if (suggestions.length === 0) return;

        const currentActive = document.querySelector('.suggestion-item.active');
        let nextIndex = 0;

        if (currentActive) {
            const currentIndex = Array.from(suggestions).indexOf(currentActive);
            nextIndex = key === 'ArrowDown' ? currentIndex + 1 : currentIndex - 1;

            if (nextIndex >= suggestions.length) nextIndex = 0;
            if (nextIndex < 0) nextIndex = suggestions.length - 1;
        }

        suggestions.forEach(item => item.classList.remove('active'));
        suggestions[nextIndex].classList.add('active');
        document.getElementById('globalSearchInput').value = suggestions[nextIndex].textContent;
    }

    showLoading() {
        const searchBtn = document.getElementById('globalSearchBtn');
        if (searchBtn) {
            const originalHtml = searchBtn.innerHTML;
            searchBtn.innerHTML = '<i class="fa fa-spinner fa-spin"></i>';
            searchBtn.disabled = true;

            // Khôi phục sau 2 giây
            setTimeout(() => {
                searchBtn.innerHTML = originalHtml;
                searchBtn.disabled = false;
            }, 2000);
        }
    }

    showNotification(message, type = 'info') {
        // Tạo thông báo tạm thời
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show`;
        notification.style.cssText = `
            position: fixed;
            top: 100px;
            right: 20px;
            z-index: 10000;
            min-width: 300px;
            animation: slideInRight 0.3s ease-out;
        `;
        notification.innerHTML = `
            <i class="fa fa-${type === 'warning' ? 'exclamation-triangle' : 'info-circle'} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(notification);

        // Tự động ẩn sau 3 giây
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 3000);
    }
}

// Khởi tạo global search
document.addEventListener('DOMContentLoaded', function () {
    window.globalSearch = new GlobalSearch();
});