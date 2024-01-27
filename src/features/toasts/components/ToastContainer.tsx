import React from 'react';
import { MdClose } from 'react-icons/md';
import { ToastContainer as ToastifyContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

import { ActionIcon } from '../../../components';
import { createToastClassName } from '../../../utils/toast';

export const ToastContainer: React.FC = () => {
  return (
    <ToastifyContainer
      position="bottom-right"
      newestOnTop
      className="flex flex-col gap-2"
      bodyClassName={() => 'text-sm font-med block p-3 gap-2 flex flex-row gap-2 w-fit'}
      toastClassName={(props) => createToastClassName(props)}
      closeButton={({ closeToast }) => (
        <ActionIcon
          icon={MdClose}
          size="xs"
          className="h-6 w-6"
          variant="ghost"
          onClick={closeToast}
        />
      )}
    />
  );
};
