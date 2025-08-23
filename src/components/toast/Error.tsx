import type React from "react";

export type ToastErrorProps = {
  title?: React.ReactNode;
  message?: React.ReactNode;
};

export const ToastError: React.FC<ToastErrorProps> = ({ title, message }) => {
  if (title) {
    return (
      <div className="flex flex-col gap-2">
        <h2 className="text-md font-bold text-gray-50">{title}</h2>
        <p className="text-sm text-gray-50">{message}</p>
      </div>
    );
  }

  return <p className="text-md text-gray-50">{message}</p>;
};
