import * as Tooltip from '@radix-ui/react-tooltip';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { HotkeysProvider } from 'react-hotkeys-hook';
import { I18nextProvider } from 'react-i18next';
import { MdClose } from 'react-icons/md';
import { RouterProvider } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

import { ActionIcon } from '.';
import i18next from '../lib/i18next';
import { queryClient } from '../lib/query';
import { router } from '../lib/router';
import { createToastClassName } from '../utils/toast';

export const App = () => {
  return (
    <QueryClientProvider client={queryClient}>
      <I18nextProvider i18n={i18next}>
        <Tooltip.Provider delayDuration={800}>
          <HotkeysProvider>
            <RouterProvider router={router} />
            <ToastContainer
              position="bottom-right"
              newestOnTop
              className="flex flex-col gap-2"
              bodyClassName={() => 'text-sm font-med block p-3 gap-2 flex flex-row gap-2'}
              toastClassName={(props) => createToastClassName(props)}
              closeButton={({ closeToast }) => (
                <ActionIcon
                  icon={MdClose}
                  size="sm"
                  className="h-6 w-6"
                  variant="ghost"
                  onClick={closeToast}
                />
              )}
            />
          </HotkeysProvider>
        </Tooltip.Provider>
      </I18nextProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
};
