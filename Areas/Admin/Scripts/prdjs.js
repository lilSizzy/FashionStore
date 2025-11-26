// === PRODUCT MANAGEMENT SCRIPT ===
// File: ~/Areas/Admin/Scripts/products.js

class ProductManager {
    constructor() {
        this.initEvents();
        this.initPriceValidation();
    }

    // Khởi tạo sự kiện
    initEvents() {
        this.initSearch();
        this.initExport();
        this.initDeleteConfirm();
        this.initQuickActions();
        this.initClearFilter();
    }

    // Xử lý tìm kiếm
    initSearch() {
        const searchBtn = document.getElementById('searchBtn');
        const resetBtn = document.getElementById('resetBtn');
        const searchForm = document.getElementById('searchForm');
        const searchTerm = document.getElementById('searchTerm');

        if (searchBtn && searchForm) {
            searchBtn.addEventListener('click', (e) => {
                this.handleSearch(e);
            });
        }

        if (resetBtn) {
            resetBtn.addEventListener('click', (e) => {
                e.preventDefault();
                window.location.href = this.getBaseUrl();
            });
        }

        // Tìm kiếm khi nhấn Enter
        if (searchTerm) {
            searchTerm.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    this.handleSearch(e);
                }
            });
        }
    }

    // Xử lý tìm kiếm
    handleSearch(e) {
        // Validate giá trước khi submit
        if (!this.validatePriceRange()) {
            e.preventDefault();
            return false;
        }

        // Hiệu ứng loading
        const searchBtn = e.target;
        const originalText = searchBtn.innerHTML;
        searchBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Đang tìm...';
        searchBtn.disabled = true;

        // Khôi phục nút sau 2 giây (phòng trường hợp có lỗi)
        setTimeout(() => {
            searchBtn.innerHTML = originalText;
            searchBtn.disabled = false;
        }, 2000);

        return true;
    }

    // Validate khoảng giá
    validatePriceRange() {
        const minPrice = document.getElementById('minPrice');
        const maxPrice = document.getElementById('maxPrice');

        if (minPrice && maxPrice) {
            const minValue = minPrice.value ? parseFloat(minPrice.value) : null;
            const maxValue = maxPrice.value ? parseFloat(maxPrice.value) : null;

            if (minValue !== null && maxValue !== null && minValue > maxValue) {
                alert('Giá "từ" không thể lớn hơn giá "đến"');
                minPrice.focus();
                return false;
            }
        }
        return true;
    }

    // Xử lý xuất Excel
    initExport() {
        const exportBtn = document.getElementById('exportExcelBtn');

        if (exportBtn) {
            exportBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.exportToExcel();
            });
        }
    }

    // Xuất Excel
    exportToExcel() {
        const exportBtn = document.getElementById('exportExcelBtn');
        const originalText = exportBtn.innerHTML;

        // Hiệu ứng loading
        exportBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-1"></i>Đang xuất...';
        exportBtn.disabled = true;

        // Lấy các tham số tìm kiếm hiện tại
        const urlParams = new URLSearchParams(window.location.search);
        const searchTerm = urlParams.get('SearchTerm') || '';
        const minPrice = urlParams.get('MinPrice') || '';
        const maxPrice = urlParams.get('MaxPrice') || '';

        // Tạo URL export
        const baseUrl = window.location.pathname;
        let exportUrl = `${baseUrl}/Export?searchTerm=${encodeURIComponent(searchTerm)}&minPrice=${encodeURIComponent(minPrice)}&maxPrice=${encodeURIComponent(maxPrice)}`;

        // Tạo link tải xuống
        const link = document.createElement('a');
        link.href = exportUrl;
        link.download = 'products.xlsx';
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        // Khôi phục trạng thái nút
        setTimeout(() => {
            exportBtn.innerHTML = originalText;
            exportBtn.disabled = false;
        }, 2000);
    }

    // Xác nhận xóa
    initDeleteConfirm() {
        const deleteLinks = document.querySelectorAll('[data-action="delete"]');

        deleteLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                const productName = link.closest('tr').querySelector('.product-name-table').textContent;
                if (!this.confirmDelete(productName)) {
                    e.preventDefault();
                }
            });
        });
    }

    // Hiển thị confirm xóa
    confirmDelete(productName) {
        return confirm(`Bạn có chắc chắn muốn xóa sản phẩm "${productName}"?`);
    }

    // Hành động nhanh
    initQuickActions() {
        const productRows = document.querySelectorAll('.product-row');

        productRows.forEach(row => {
            // Hiệu ứng hover
            row.addEventListener('mouseenter', () => {
                row.style.backgroundColor = '#f8f9fc';
                row.style.cursor = 'pointer';
            });

            row.addEventListener('mouseleave', () => {
                row.style.backgroundColor = '';
            });

            // Click để edit
            row.addEventListener('click', (e) => {
                if (!e.target.closest('.action-buttons')) {
                    const editLink = row.querySelector('[data-action="edit"]');
                    if (editLink) {
                        window.location.href = editLink.href;
                    }
                }
            });
        });
    }

    // Xử lý nút xóa bộ lọc
    initClearFilter() {
        const clearFilterBtn = document.getElementById('clearFilterBtn');
        if (clearFilterBtn) {
            clearFilterBtn.addEventListener('click', (e) => {
                e.preventDefault();
                window.location.href = this.getBaseUrl();
            });
        }
    }

    // Validate giá nhập vào
    initPriceValidation() {
        const priceInputs = document.querySelectorAll('#minPrice, #maxPrice');

        priceInputs.forEach(input => {
            // Khi blur, format số
            input.addEventListener('blur', (e) => {
                let value = e.target.value;
                if (value && !isNaN(value)) {
                    e.target.value = parseInt(value).toLocaleString('vi-VN');
                }
            });

            // Khi focus, bỏ format
            input.addEventListener('focus', (e) => {
                let value = e.target.value;
                value = value.replace(/\./g, '');
                e.target.value = value;
            });
        });
    }

    // Lấy base URL
    getBaseUrl() {
        return window.location.pathname;
    }

    // Hiển thị thông báo
    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show`;
        notification.innerHTML = `
            ${message}
            <button type="button" class="close" data-dismiss="alert">
                <span>&times;</span>
            </button>
        `;

        const container = document.querySelector('.container-fluid');
        if (container) {
            container.prepend(notification);

            // Tự động ẩn sau 5 giây
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 5000);
        }
    }
}

// Global functions
function confirmDelete(productName) {
    if (window.productManager) {
        return window.productManager.confirmDelete(productName);
    }
    return confirm(`Bạn có chắc chắn muốn xóa sản phẩm "${productName}"?`);
}

// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', function () {
    window.productManager = new ProductManager();

    // Thêm custom styles
    const style = document.createElement('style');
    style.textContent = `
        .product-row:hover {
            transition: background-color 0.3s ease;
        }
        .action-buttons {
            display: flex;
            gap: 5px;
            justify-content: center;
        }
        .action-buttons .btn {
            padding: 6px 12px;
            border-radius: 4px;
            font-size: 14px;
            transition: all 0.3s ease;
        }
        .action-buttons .btn:hover {
            transform: translateY(-1px);
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }
    `;
    document.head.appendChild(style);
});