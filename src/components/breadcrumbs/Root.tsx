import clsx from 'clsx';
import React from 'react';

import { isElement } from '../../utils';
import { BreadcrumbItem } from './Item';

export type BreadcrumbsRootProps = {
  separator?: React.ReactNode;

  className?: string;
  children?: React.ReactNode;
};

export const BreadcrumbsRoot: React.FC<BreadcrumbsRootProps> = ({
  separator = '/',
  className,
  children,
}) => {
  const items = React.Children.toArray(children).reduce<React.ReactNode[]>(
    (acc, child, index, array) => {
      acc.push(
        isElement(child) ? (
          React.cloneElement(child, { key: index })
        ) : (
          <BreadcrumbItem key={index} href="">
            {child}
          </BreadcrumbItem>
        ),
      );

      if (index + 1 !== array.length) {
        acc.push(
          <div key={`${index}-separator`} className="text-md text-gray-50">
            {separator}
          </div>,
        );
      }

      return acc;
    },
    [],
  );

  return <div className={clsx(className, 'flex flex-row gap-1')}>{items}</div>;
};
