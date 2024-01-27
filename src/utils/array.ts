export const createArrayRange = (length: number, start: number) =>
  [...Array(length).keys()].map((_, i) => i + start);
