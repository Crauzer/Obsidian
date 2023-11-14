import clsx from 'clsx';
import { match } from 'ts-pattern';

import { ClickableVariant, JustifyContent, Size } from '../types';

export const getJustifyClass = (justify: JustifyContent) =>
  match(justify)
    .with('center', () => 'justify-center')
    .with('flex-end', () => 'justify-end')
    .with('flex-start', () => 'justify-start')
    .with('space-around', () => 'justify-around')
    .with('space-between', () => 'justify-between')
    .with('space-evenly', () => 'justify-evenly')
    .exhaustive();

export const getTextSizeClass = (size: Size) =>
  match(size)
    .with('xs', () => 'text-xs')
    .with('sm', () => 'text-sm')
    .with('md', () => 'text-md')
    .with('lg', () => 'text-lg')
    .with('xl', () => 'text-xl')
    .exhaustive();

export const getClickableVariantClass = (variant: ClickableVariant) =>
  clsx({
    'bg-gray-800 border-gray-600 text-gray-50 hover:bg-gray-700 hover:border-gray-500 active:bg-gray-600':
      variant === 'default',
    'bg-obsidian-800 border-obsidian-600 text-gray-50 hover:bg-obsidian-900 hover:border-obsidian-700':
      variant === 'filled',
    'border-obsidian-500/60 border text-gray-50 hover:bg-obsidian-500/60': variant === 'outline',
    'bg-obsidian-600/40 text-gray-50 hover:bg-obsidian-600/60 border-none': variant === 'light',
    'text-gray-200 hover:bg-obsidian-600/30 border-none': variant === 'ghost',
  });
