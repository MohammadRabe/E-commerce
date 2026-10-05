import { useEffect, useMemo, useState } from "react";
import ProductCard from "../components/ProductCard/ProductCard";
import { Link } from "react-router-dom";
import SARPrice from "../components/SARPrice";
import { api, getApiErrorMessage, normalizeProduct } from "../api";
import { Spin } from "antd";
import { LoadingOutlined } from "@ant-design/icons";
import { formatSAR } from "../formatCurrency";

const MAX_PRICE = 100000;
const PAGE_SIZE = 30;
const categoryNames = { all: "الكل", electronics: "إلكترونيات", jewelery: "مجوهرات", "men's clothing": "أزياء رجالية", "women's clothing": "أزياء نسائية", general: "متنوع" };
const categoryLabel = (value) => categoryNames[value.toLowerCase()] || value;
const shuffleProducts = (items) => {
  const shuffled = [...items];
  for (let index = shuffled.length - 1; index > 0; index -= 1) {
    const randomIndex = Math.floor(Math.random() * (index + 1));
    [shuffled[index], shuffled[randomIndex]] = [shuffled[randomIndex], shuffled[index]];
  }
  return shuffled;
};

function Home({ addToCart, search, setSearch, likedProductIds = [], toggleLike, isAdmin = false, onDeleteProduct }) {
  const [products, setProducts] = useState([]);
  const [productPage, setProductPage] = useState({ pageNumber: 1, pageSize: PAGE_SIZE, totalCount: 0, next: false, prev: false });
  const [availableCategories, setAvailableCategories] = useState([]);
  const [category, setCategory] = useState("all");
  const [maxPrice, setMaxPrice] = useState(MAX_PRICE);
  const [sort, setSort] = useState("featured");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [activeProductIndex, setActiveProductIndex] = useState(0);
  const [carouselPaused, setCarouselPaused] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const featuredProducts = useMemo(() => [...products]
    .sort((a, b) => (b.rating?.rate || 0) - (a.rating?.rate || 0))
    .slice(0, 5), [products]);

  useEffect(() => {
    if (carouselPaused || featuredProducts.length < 2) return undefined;
    const timer = window.setInterval(() => setActiveProductIndex((current) => (current + 1) % featuredProducts.length), 3400);
    return () => window.clearInterval(timer);
  }, [carouselPaused, featuredProducts.length]);
  useEffect(() => {
    let active = true;
    setLoading(true);
    setError("");
    api.productPage(pageNumber, PAGE_SIZE).then((data) => {
      if (!active) return;
      const items = data?.items ?? data?.Items ?? [];
      setProducts(shuffleProducts(items.map(normalizeProduct)));
      setProductPage({
        pageNumber: Number(data?.pageNumber ?? data?.PageNumber) || pageNumber,
        pageSize: Number(data?.pageSize ?? data?.PageSize) || PAGE_SIZE,
        totalCount: Number(data?.totalCount ?? data?.TotalCount) || 0,
        next: Boolean(data?.next ?? data?.Next),
        prev: Boolean(data?.prev ?? data?.Prev),
      });
    }).catch((reason) => {
      if (active) setError(getApiErrorMessage(reason, "ما قدرنا نحمّل المنتجات الحين. تأكد من اتصالك وحاول مرة ثانية."));
    }).finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [pageNumber]);

  useEffect(() => {
    setPageNumber(1);
  }, [category, maxPrice, sort]);

  useEffect(() => {
    let active = true;
    api.categories()
      .then((data) => {
        if (active) setAvailableCategories((data?.items ?? data ?? []).map((item) => item.name).filter(Boolean));
      })
      .catch(() => {});
    return () => { active = false; };
  }, []);


  const categories = useMemo(() => {
    const names = new Map();
    [...availableCategories, ...products.map((product) => product.category)].filter(Boolean).forEach((name) => {
      const key = name.trim().toLocaleLowerCase();
      if (key && !names.has(key)) names.set(key, name.trim());
    });
    return [...names.values()];
  }, [availableCategories, products]);

  const filteredProducts = products.filter((product) => {
    const matchesCategory =
      category === "all" || product.category === category;

    const matchesPrice = product.price <= maxPrice;

    return matchesCategory && matchesPrice;
  }).sort((a, b) => {
    if (sort === "price-low") return a.price - b.price;
    if (sort === "price-high") return b.price - a.price;
    if (sort === "rating") return (b.rating?.rate || 0) - (a.rating?.rate || 0);
    return 0;
  });

  const handleDeleteProduct = async (product) => {
    const deleted = await onDeleteProduct?.(product);
    if (!deleted) return;
    if (products.length === 1 && pageNumber > 1) setPageNumber((page) => page - 1);
    else setProducts((current) => current.filter((item) => Number(item.id) !== Number(product.id)));
    setProductPage((current) => ({ ...current, totalCount: Math.max(0, current.totalCount - 1) }));
  };

  const pageCount = Math.max(1, Math.ceil(productPage.totalCount / PAGE_SIZE));
  const visiblePages = Array.from({ length: Math.min(pageCount, 5) }, (_, index) => {
    const start = Math.min(Math.max(1, pageNumber - 2), Math.max(1, pageCount - 4));
    return start + index;
  });
  const goToPage = (page) => {
    setPageNumber(Math.max(1, Math.min(pageCount, page)));
    document.getElementById("products")?.scrollIntoView({ behavior: "smooth", block: "start" });
  };

  return (
    <>
      <div className="market-home-intro container">
        {featuredProducts.length > 0 && <section className="market-promo" aria-label="منتجات مختارة" onMouseEnter={() => setCarouselPaused(true)} onMouseLeave={() => setCarouselPaused(false)} onFocus={() => setCarouselPaused(true)} onBlur={(event) => { if (!event.currentTarget.contains(event.relatedTarget)) setCarouselPaused(false); }}>
          <div key={featuredProducts[activeProductIndex]?.id} className="market-promo-card market-product-slide" aria-live="polite">
            {(() => {
              const product = featuredProducts[activeProductIndex] || featuredProducts[0];
              return <>
                <Link className="market-product-image" to={`/product/${product.id}`} aria-label={`عرض ${product.title}`} style={{ backgroundImage: product.image ? `url(${JSON.stringify(product.image)})` : undefined }} />
                <div className="market-product-copy">
                  <span className="market-promo-eyebrow"><i className="bi bi-stars" aria-hidden="true" /> {categoryLabel(product.category)}</span>
                  <div className="market-product-info-line"><Link to={`/product/${product.id}`}><h2>{product.title}</h2></Link><strong className="market-product-price"><SARPrice value={product.price} /></strong></div>
                  <div className="market-product-actions"><Link className="market-promo-link" to={`/product/${product.id}`}>عرض المنتج<i className="bi bi-arrow-left" aria-hidden="true" /></Link><button type="button" onClick={() => addToCart(product)} aria-label={`أضف ${product.title} للسلة`}><i className="bi bi-cart-plus" aria-hidden="true" /></button></div>
                </div>
              </>;
            })()}
            <div className="market-promo-controls" aria-label="التنقل بين المنتجات المختارة">
              {featuredProducts.map((product, index) => <button key={product.id} type="button" className={index === activeProductIndex ? "is-active" : ""} onClick={() => setActiveProductIndex(index)} aria-label={`المنتج ${index + 1}`} aria-pressed={index === activeProductIndex} />)}
            </div>
          </div>
        </section>}
        <div className="market-collection-intro">
          <div><p className="section-kicker mb-2">Sooq</p><h1 className="section-title mb-1">كل المنتجات</h1><p>تصفّح تشكيلتنا واختر ما يناسبك.</p></div>
          <div className="collection-count"><strong>{productPage.totalCount}</strong><span>منتج<br />بانتظارك</span></div>
        </div>
      </div>
      <section id="products" className="container collection-section">
        <div className="shop-toolbar">
          <div className="shop-tools">
            <select className="form-select shop-category-select" aria-label="اختر القسم" value={category} onChange={(e) => setCategory(e.target.value)}><option value="all">كل الأقسام</option>{categories.map((item) => <option key={item} value={item}>{categoryLabel(item)}</option>)}</select>
            <select className="form-select sort-select" aria-label="ترتيب المنتجات" value={sort} onChange={(e) => setSort(e.target.value)}><option value="featured">ترتيب حسب</option><option value="price-low">السعر: من الأقل للأعلى</option><option value="price-high">السعر: من الأعلى للأقل</option><option value="rating">الأعلى تقييماً</option></select>
            <details className="price-filter"><summary><i className="bi bi-sliders2" /> السعر</summary><div className="price-popover"><label htmlFor="max-price">الحد الأعلى <strong>{formatSAR(maxPrice)}</strong></label><input id="max-price" type="range" className="form-range" min="0" max={MAX_PRICE} step="100" value={maxPrice} onChange={(e) => setMaxPrice(Number(e.target.value))} /><button type="button" className="btn btn-sm btn-link p-0" onClick={() => setMaxPrice(MAX_PRICE)}>إعادة ضبط السعر</button></div></details>
          </div>
        </div>
        <div className="results-line"><span>{filteredProducts.length} منتج</span><span className="results-divider" /><span>أشياء تستاهل، وأسعار تناسبك</span></div>

            {error && <div className="alert alert-warning">{error}</div>}
           { loading ? ( 
            <div className="d-flex justify-content-center align-items-center py-5"> 
              <Spin indicator={ <LoadingOutlined style={{ fontSize: 48, color: "#ff9900", }} spin /> } /> 
            </div>
            )
            : filteredProducts.length === 0 ? 
                ( 
                <div className="alert alert-warning">ما لقينا منتجات تطابق بحثك. جرّب تغيّر خيارات التصفية.</div> )
                  : 
                  ( 
                  <div className="row g-3 g-xl-4 product-grid"> 
                    {filteredProducts.map((product) => ( 
                      <div className="col-6 col-md-4 col-xl-3" key={product.id} >
                          <ProductCard product={product} addToCart={addToCart} liked={likedProductIds.includes(Number(product.id))} onToggleLike={toggleLike} isAdmin={isAdmin} onDeleteProduct={handleDeleteProduct} /> 
                      </div> ))
                    } 
                  </div> ) }
        {!loading && !error && productPage.totalCount > 0 && <nav className="product-pagination" aria-label="صفحات المنتجات">
          <span>صفحة {pageNumber} من {pageCount}</span>
          <div className="product-pagination-pages">
            <button type="button" onClick={() => goToPage(pageNumber - 1)} disabled={!productPage.prev} aria-label="الصفحة السابقة"><i className="bi bi-chevron-right" /> السابقة</button>
            {visiblePages.map((page) => <button key={page} type="button" className={page === pageNumber ? "active" : ""} onClick={() => goToPage(page)} aria-current={page === pageNumber ? "page" : undefined}>{page}</button>)}
            <button type="button" onClick={() => goToPage(pageNumber + 1)} disabled={!productPage.next} aria-label="الصفحة التالية">التالية <i className="bi bi-chevron-left" /></button>
          </div>
        </nav>}
      </section>

    </>
  );
}

export default Home;
