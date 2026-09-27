const sarFormatter = new Intl.NumberFormat("ar-SA", {
  style: "currency",
  currency: "SAR",
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

export function formatSAR(value) {
  return sarFormatter.format(Number(value) || 0);
}
