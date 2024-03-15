import { MutationCache, QueryCache, QueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';

import { Toast } from '../components';
import { apiErrorSchema } from '../types/error';

export const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: false, refetchOnWindowFocus: false, staleTime: 100_000 } },
  queryCache: new QueryCache({
    onError: (error) => {
      console.error(error);

      const apiError = apiErrorSchema.safeParse(error);
      if (apiError.success) {
        toast.error(<Toast.Error title={apiError.data.title} message={apiError.data.message} />);
      }
    },
  }),
  mutationCache: new MutationCache({
    onError: (error) => {
      console.error(error);

      const apiError = apiErrorSchema.safeParse(error);
      if (apiError.success) {
        toast.error(<Toast.Error title={apiError.data.title} message={apiError.data.message} />);
      }
    },
  }),
});
