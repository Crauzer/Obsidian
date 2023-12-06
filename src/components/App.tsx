import * as Tooltip from '@radix-ui/react-tooltip';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import clsx from 'clsx';
import { I18nextProvider } from 'react-i18next';
import { MdClose } from 'react-icons/md';
import { RouterProvider } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { match } from 'ts-pattern';

import { ActionIcon } from '.';
import i18next from '../lib/i18next';
import { queryClient } from '../lib/query';
import { router } from '../lib/router';

export const App = () => {
  return (
    <QueryClientProvider client={queryClient}>
      <I18nextProvider i18n={i18next}>
        <Tooltip.Provider delayDuration={800}>
          <RouterProvider router={router} />
          <ToastContainer
            position="bottom-right"
            newestOnTop
            className="flex flex-col gap-2"
            bodyClassName={() => 'text-sm font-med block p-3 gap-2 flex flex-row gap-2'}
            toastClassName={(props) => {
              return clsx(
                'relative flex p-1 min-h-10 rounded-md border justify-between overflow-hidden cursor-pointer backdrop-blur ',
                match(props?.type)
                  .with('default', () => 'bg-gray-700/40 border-gray-600/25')
                  .with('info', () => 'bg-blue-700/40 border-blue-600/25')
                  .with('success', () => 'bg-green-700/40 border-green-600/25')
                  .with('error', () => 'bg-obsidian-700/40 border-obsidian-600/25')
                  .with('warning', () => 'bg-yellow-700/40 border-yellow-600/25')
                  .otherwise(() => 'bg-gray-700/40 border-gray-600/25'),
              );
            }}
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
        </Tooltip.Provider>
      </I18nextProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
};
