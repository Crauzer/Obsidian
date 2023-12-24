import qs from 'qs';

export const composeUrlQuery = <TQuery>(url: string, query: TQuery) =>
  `${url}?${qs.stringify(query)}`;
