import React from 'react';
import { ToastOptions } from 'react-toastify';
import { toast as toastify } from 'react-toastify';

import { Toast } from '../components';

const info = (message: React.ReactNode, options?: ToastOptions) => {
  return toastify.info(<Toast.Info message={message} />, options);
};

export const toast = {
  info,
};
