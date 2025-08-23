import * as Tooltip from "@radix-ui/react-tooltip";
import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { HotkeysProvider } from "react-hotkeys-hook";
import { I18nextProvider } from "react-i18next";
import { RouterProvider } from "react-router-dom";
import "react-toastify/dist/ReactToastify.css";

import { ToastContainer } from "../features/toasts";
import i18next from "../lib/i18next";
import { queryClient } from "../lib/query";
import { router } from "../lib/router";

export const App = () => {
  return (
    <QueryClientProvider client={queryClient}>
      <I18nextProvider i18n={i18next}>
        <Tooltip.Provider delayDuration={800}>
          <HotkeysProvider>
            <RouterProvider router={router} />
            <ToastContainer />
          </HotkeysProvider>
        </Tooltip.Provider>
      </I18nextProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
};
