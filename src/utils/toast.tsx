import clsx from 'clsx';
import { TypeOptions } from 'react-toastify';
import { match } from 'ts-pattern';

export const toastAutoClose = {
  infinite: false,
  default: 5000,
  short: 2500,
};

export const createToastClassName = (props: { type?: TypeOptions } | undefined) =>
  clsx(
    'relative flex p-1 min-h-10 rounded-md border justify-between overflow-hidden cursor-pointer backdrop-blur ',
    match(props?.type)
      .with('default', () => 'bg-gray-700/40 border-gray-600/25')
      .with('info', () => 'bg-blue-700/40 border-blue-600/25')
      .with('success', () => 'bg-green-700/40 border-green-600/25')
      .with('error', () => 'bg-obsidian-700/40 border-obsidian-600/25')
      .with('warning', () => 'bg-yellow-700/40 border-yellow-600/25')
      .otherwise(() => 'bg-gray-700/40 border-gray-600/25'),
  );
