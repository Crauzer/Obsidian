import qs from 'qs';

export const composeUrlQuery = (url: string, query: any) => `${url}?${qs.stringify(query)}`;
