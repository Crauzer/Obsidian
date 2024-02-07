import clsx from 'clsx';
import { useCombobox } from 'downshift';
import React, { forwardRef, useState } from 'react';
import { MdDataArray, MdSearch } from 'react-icons/md';
import { useDebounceCallback } from 'usehooks-ts';

import { getLeagueFileKindIcon, getLeagueFileKindIconColor, useSearchWad } from '../..';
import { Icon, Input, Popover } from '../../../../components';

export type WadSearchInputProps = { wadId: string };

export const WadSearchInput = forwardRef<HTMLInputElement, WadSearchInputProps>(
  ({ wadId }, ref) => {
    const [searchQuery, setSearchQuery] = useState('');
    const handleSearchQueryChange = useDebounceCallback(setSearchQuery, 500);

    const searchWad = useSearchWad({ wadId, query: searchQuery });

    const { inputValue, getInputProps, getMenuProps, isOpen, openMenu, closeMenu, getItemProps } =
      useCombobox({
        onInputValueChange: (changes) => {
          handleSearchQueryChange(changes.inputValue ?? '');
        },
        itemToString: (item) => item?.path ?? '',
        items: searchWad.data?.items ?? [],
      });

    return (
      <Popover.Root
        open={isOpen}
        onOpenChange={(open) => {
          open ? openMenu() : closeMenu();
        }}
      >
        <Popover.Anchor className="flex h-full">
          <Input
            {...getInputProps()}
            ref={ref}
            className="w-[500px]"
            placeholder="Search"
            right={<Icon size="md" icon={MdSearch} />}
          />
        </Popover.Anchor>
        <Popover.Content side="bottom" sideOffset={6} align="end" className="min-w-[500px]">
          {isOpen && searchWad.isSuccess && searchWad.data.items.length > 0 && (
            <ul {...getMenuProps()} className="flex flex-col">
              {isOpen &&
                inputValue !== '' &&
                searchWad.data?.items.map((item, index) => (
                  <li
                    {...getItemProps({
                      item,
                      index,
                    })}
                    className="flex flex-row items-center gap-2 rounded p-1 py-2 transition-colors duration-150 hover:cursor-pointer hover:bg-gray-600 hover:shadow-inner"
                  >
                    <Icon
                      className={clsx(getLeagueFileKindIconColor(item.extensionKind))}
                      icon={getLeagueFileKindIcon(item.extensionKind)}
                    />
                    <p className="text-base">{item.path}</p>
                  </li>
                ))}
            </ul>
          )}
          {isOpen && searchWad.data?.items.length === 0 && (
            <div className="flex items-center justify-center">
              <Icon size="xl" icon={MdDataArray} />
            </div>
          )}
        </Popover.Content>
      </Popover.Root>
    );
  },
);
