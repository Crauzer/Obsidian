import type React from "react";

export type ToastInfoProps = {
  message: React.ReactNode;
};

export const ToastInfo: React.FC<ToastInfoProps> = ({ message }) => {
  return message;
};
