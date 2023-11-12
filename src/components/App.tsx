import * as Tooltip from '@radix-ui/react-tooltip';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { I18nextProvider } from 'react-i18next';
import { RouterProvider } from 'react-router-dom';

import i18next from '../lib/i18next';
import { queryClient } from '../lib/query';
import { router } from '../lib/router';

export const App = () => {
  return (
    <QueryClientProvider client={queryClient}>
      <I18nextProvider i18n={i18next}>
        <Tooltip.Provider delayDuration={350}>
          <RouterProvider router={router} />
        </Tooltip.Provider>
      </I18nextProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
};
