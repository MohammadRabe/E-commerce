function SARPrice({ value }) {
  const parts = new Intl.NumberFormat("ar-SA", {
    style: "currency",
    currency: "SAR",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).formatToParts(Number(value) || 0);

  return <span className="sar-price">{parts.map((part, index) => part.type === "currency"
    ? <span className="sar-price-currency" key={index}>{part.value}</span>
    : <span key={index}>{part.value}</span>)}</span>;
}

export default SARPrice;
