export function GetParam(
  o: any,
  searchParam = new URLSearchParams(),
  key: string | null = null
) {
  Object.entries(o).forEach(([k, v]) => {
    if (v !== null && typeof v === "object") GetParam(v, searchParam, k);
    else searchParam.append(`${key ? key + "." : ""}${k}`, v as string);
  });

  return searchParam;
}
