import { forwardRef } from 'react';
import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { VscSaveAll } from 'react-icons/vsc';
import { v4 as uuidv4 } from 'uuid';

import { useExtractWadItemsWithDirectory, useWadParentItems } from '../../..';
import { ContextMenu, Icon, LoadingOverlay } from '../../../../../components';
import { useActionProgress } from '../../../../actions';
import { WadItem } from '../../../types';

type ExtractSelectedItemProps = {
  wadId: string;
  parentItemId?: string;
  item: WadItem;
};

export const ExtractSelectedItem = forwardRef<HTMLDivElement, ExtractSelectedItemProps>(
  ({ wadId, parentItemId, item }, ref) => {
    const [t] = useTranslation('wad');

    const [actionId] = useState<string>(uuidv4());

    const parentItems = useWadParentItems({ wadId, parentId: parentItemId });
    const { isExtracting, extractWadItemsWithDirectory } = useExtractWadItemsWithDirectory();
    const actionProgress = useActionProgress(actionId);

    const selectedItems = useMemo(() => {
      return parentItems.data?.filter((x) => x.isSelected);
    }, [parentItems.data]);

    const progress = useMemo(() => {
      if (actionProgress.isSuccess) {
        return actionProgress.data?.payload.progress;
      }

      return 0;
    }, [actionProgress.data?.payload.progress, actionProgress.isSuccess]);

    return (
      <>
        <ContextMenu.Item
          disabled={!selectedItems || selectedItems.length <= 1}
          ref={ref}
          className="flex flex-row items-center gap-2"
          onClick={() => {
            if (!selectedItems || selectedItems.length <= 1) {
              return;
            }

            extractWadItemsWithDirectory({
              wadId,
              parentItemId,
              items: selectedItems?.map((item) => item.id),
              actionId,
            });
          }}
          onSelect={(e) => e.preventDefault()}
        >
          <Icon icon={VscSaveAll} size="md" />
          {t('contextMenu.extractSelected')}
        </ContextMenu.Item>
        <LoadingOverlay
          open={isExtracting}
          onOpenChange={() => {}}
          progress={progress * 100}
          message={actionProgress.data?.payload.message}
        />
      </>
    );
  },
);
